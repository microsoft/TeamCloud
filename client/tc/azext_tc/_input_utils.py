# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=too-many-lines, too-many-locals, too-many-statements

from collections import OrderedDict

from knack.log import get_logger
# from knack.prompting import prompt_pass
from knack.prompting import (prompt, prompt_t_f, prompt_choice_list,
                             prompt_int, NoTTYException)
from knack.util import CLIError
from azure.cli.core.util import shell_safe_json_parse

from ._prompting import prompt_number, prompt_multi_choice_list

logger = get_logger(__name__)

# used resource group parameter code as starting reference:
# https://github.com/Azure/azure-cli/blob/dev/src/azure-cli/azure/cli/command_modules/resource/custom.py


def _get_best_match_one_of(input_schema, parameter_lists):

    def _get_parameter_keys(parameters_list):
        parameter_keys = []
        for params in parameters_list or []:
            for item in params:
                try:
                    key, _ = item.split('=', 1)
                    parameter_keys.append(key)
                except ValueError:
                    logger.debug('ignoring paramater: %s', item)
        return parameter_keys

    def _get_matching_properties(schema, keys):
        matches = 0
        prop_names = []
        props_schema = schema.get('properties', None)
        if props_schema is not None:
            for prop_name in props_schema:
                prop_names.append(prop_name)
                if prop_name in keys:
                    matches = matches + 1
        return matches, prop_names

    one_of_schemas = input_schema.get('oneOf', None)
    if one_of_schemas is None:
        raise CLIError('input_schema does not have oneOf')

    parameter_keys = _get_parameter_keys(parameter_lists)

    best_match = None
    best_matches = 0

    property_names_list = [[]]

    for one_of_schema in one_of_schemas:
        matching_properties, property_names = _get_matching_properties(one_of_schema, parameter_keys)
        property_names_list.append(property_names)
        if best_match is None or matching_properties > best_matches:
            best_match = one_of_schema
            best_matches = matching_properties

    if best_matches == 0 or best_match is None:
        one_of_titles = [s.get('title', False) for s in one_of_schemas]
        if all(one_of_titles):
            prompt_str = 'Please choose one of the following parameter sets to provide: '
            while True:
                try:
                    ix = prompt_choice_list(prompt_str, one_of_titles)
                    best_match = one_of_schemas[ix]
                except NoTTYException as e:
                    raise CLIError('--parameters missing required values: {}'
                                   .format(' | '.join([', '.join(nms) for nms in property_names_list if nms]))) from e
                break
        else:
            raise CLIError('--parameters missing required values: {}'
                           .format(' | '.join([', '.join(nms) for nms in property_names_list if nms])))

    return best_match


def _get_properties_schema(input_schema):
    properties_schema = input_schema.get('properties', None)

    if properties_schema is None:
        raise CLIError('unable to get properties from input schema')

    return properties_schema


def _get_property_schema(properties_schema, property_name):
    property_schema = properties_schema.get(property_name, None)

    if property_schema is None:
        return None, None
        # raise CLIError('unable to get properties from input schema')

    property_type = _get_property_type(property_schema, property_name)

    return property_schema, property_type


def _get_property_type(property_schema, property_name=None):
    property_name = property_schema.get('title', '') if property_name is None else property_name
    property_types = property_schema.get('type', None)

    if property_types is None:
        raise CLIError("unable to resolve the type for input paramater '{}'".format(property_name))

    # The type keyword may either be a string or an array
    # https://json-schema.org/understanding-json-schema/reference/type.html#type-specific-keywords
    property_type = property_types if isinstance(property_types, str) \
        else next((t for t in property_types if t.lower() != 'null'), None)

    if property_type is None:
        raise CLIError("unable to resolve the type for input paramater '{}'".format(property_name))

    property_type = property_type.lower()

    return property_type


def _get_property_enums(property_schema):
    property_enums = property_schema.get('enum', None)
    property_enum_names = property_schema.get('enumNames', None)

    if property_enums is not None and property_enum_names is not None:
        allowed_values = [{
            'name': property_enum_names[index],
            'desc': item
        } for index, item in enumerate(property_enums)]
    else:
        allowed_values = property_enums

    return property_enums, allowed_values


def _process_parameters(input_schema, parameter_lists):  # pylint: disable=too-many-branches

    # def _try_parse_json_object(value):
    #     try:
    #         parsed = _remove_comments_from_json(value, False)
    #         return parsed.get('parameters', parsed)
    #     except Exception:  # pylint: disable=broad-except
    #         return None

    def _try_parse_key_value_object(properties_schema, parameters, value, addtl_properties=False):
        # support situation where empty JSON "{}" is provided
        if value == '{}' and not parameters:
            return True

        try:
            key, value = value.split('=', 1)
        except ValueError:
            return False

        property_schema, property_type = _get_property_schema(properties_schema, key)

        if property_schema is None:
            if addtl_properties:
                property_type = 'string'
            else:
                raise CLIError("unrecognized parameter '{}'. Allowed parameters: {}"
                               .format(key, ', '.join(sorted(properties_schema.keys()))))

        if property_type in ['object', 'array']:
            parameters[key] = shell_safe_json_parse(value)
        elif property_type == 'string':
            parameters[key] = value
        elif property_type == 'boolean':
            parameters[key] = value.lower() == 'true'
        elif property_type == 'integer':
            parameters[key] = int(value)
        elif property_type == 'number':
            parameters[key] = float(value)
        else:
            logger.warning("Unrecognized type '%s' for parameter '%s'. Interpretting as string.", property_type, key)
            parameters[key] = value

        return True

    def _try_set_defaults(properties_schema, parameters):
        for property_name in properties_schema:
            input_property = properties_schema[property_name]
            if property_name not in parameters and 'default' in input_property:
                param_default = input_property.get('default', None)
                if param_default is None:
                    raise CLIError("unable to get default value for paramater '{}'".format(property_name))
                parameters[property_name] = param_default

    properties_schema = _get_properties_schema(input_schema)

    # support additionalProperties = true
    # https://json-schema.org/understanding-json-schema/reference/object.html#additional-properties
    additional_properties = input_schema.get('additionalProperties', False)

    parameters = {}
    for params in parameter_lists or []:
        for item in params:
            if not _try_parse_key_value_object(properties_schema, parameters, item, additional_properties):
                raise CLIError('Unable to parse parameter: {}'.format(item))

    _try_set_defaults(properties_schema, parameters)

    return parameters


# pylint: disable=redefined-outer-name
def _find_missing_parameters(parameters, input_schema):
    if input_schema is None:
        return {}
    properties_schema = _get_properties_schema(input_schema)

    if properties_schema is None:
        return {}

    required_properties = input_schema.get('required', None)

    missing = OrderedDict()
    for property_name in properties_schema:
        property_schema = properties_schema[property_name]
        if parameters is not None and parameters.get(property_name, None) is not None:
            continue
        if required_properties is not None and property_name not in required_properties:
            continue
        missing[property_name] = property_schema
    return missing


def _prompt_for_parameters(missing_parameters, ui_schema=None, fail_on_no_tty=True):  # pylint: disable=too-many-branches

    prompt_list = missing_parameters.keys() if isinstance(missing_parameters, OrderedDict) \
        else sorted(missing_parameters)
    result = OrderedDict()
    no_tty = False
    for property_name in prompt_list:
        property_schema = missing_parameters[property_name]

        ui = ui_schema.get(property_name, None) if ui_schema is not None else None

        property_type = _get_property_type(property_schema, property_name)

        title = ui.get('ui:title', None) if ui is not None else None
        if title is None:
            title = property_schema.get('title', property_name)

        description = ui.get('ui:description', None) if ui is not None else None
        if description is None:
            description = property_schema.get('title', 'Missing description')  # TODO: maybe use title

        property_enums, allowed_values = _get_property_enums(property_schema)

        prompt_str = "Please provide {} value for '{}' (? for help): ".format(property_type, title)
        while True:
            if allowed_values is not None:
                try:
                    ix = prompt_choice_list(prompt_str, allowed_values, help_string=description)
                    result[property_name] = property_enums[ix]
                except NoTTYException:
                    result[property_name] = None
                    no_tty = True
                break
            # if param_type == 'securestring':
            #     try:
            #         value = prompt_pass(prompt_str, help_string=description)
            #     except NoTTYException:
            #         value = None
            #         no_tty = True
            #     result[param_name] = value
            #     break
            if property_type == 'integer':
                try:
                    int_value = prompt_int(prompt_str, help_string=description)
                    result[property_name] = int_value
                except NoTTYException:
                    result[property_name] = 0
                    no_tty = True
                break
            if property_type == 'number':
                try:
                    number_value = prompt_number(prompt_str, help_string=description)
                    result[property_name] = number_value
                except NoTTYException:
                    result[property_name] = 0
                    no_tty = True
                break
            if property_type == 'boolean':
                try:
                    value = prompt_t_f(prompt_str, help_string=description)
                    result[property_name] = value
                except NoTTYException:
                    result[property_name] = False
                    no_tty = True
                break
            property_items_schema, _ = _get_property_schema(property_schema, 'items')
            if property_type == 'array' and property_items_schema is not None:
                try:
                    property_items_schema, _ = _get_property_schema(property_schema, 'items')
                    property_items_enums, items_allowed_values = _get_property_enums(property_items_schema)
                    ixs = prompt_multi_choice_list(prompt_str, items_allowed_values, help_string=description)
                    selected_enums = []
                    for i in ixs:
                        selected_enums.append(property_items_enums[i])
                    result[property_name] = selected_enums
                except NoTTYException:
                    value = []
                break
            if property_type in ['object', 'array']:
                try:
                    value = prompt(prompt_str, help_string=description)
                except NoTTYException:
                    value = ''
                    no_tty = True

                if value == '':
                    value = {} if property_type == 'object' else []
                else:
                    try:
                        value = shell_safe_json_parse(value)
                    except Exception as ex:  # pylint: disable=broad-except
                        logger.error(ex)
                        continue
                result[property_name] = value
                break

            try:
                result[property_name] = prompt(prompt_str, help_string=description)
            except NoTTYException:
                result[property_name] = None
                no_tty = True
            break
    if no_tty and fail_on_no_tty:
        raise NoTTYException
    return result


# pylint: disable=redefined-outer-name
def _get_missing_parameters(parameters, input_schema, prompt_fn, ui_schema=None, no_prompt=False):
    missing = _find_missing_parameters(parameters, input_schema)
    if missing:
        if no_prompt is True:
            logger.warning("Missing input parameters: %s ", ', '.join(sorted(missing.keys())))
        else:
            try:
                prompt_parameters = prompt_fn(missing, ui_schema)
                for param_name in prompt_parameters:
                    parameters[param_name] = prompt_parameters[param_name]
            except NoTTYException as e:
                raise CLIError('Missing input parameters: {}'
                               .format(', '.join(sorted(missing.keys())))) from e
    return parameters

    # pylint: disable=redefined-outer-name


# def _remove_comments_from_json(template, preserve_order=True, file_path=None):
#     from jsmin import jsmin

#     # When commenting at the bottom of all elements in a JSON object, jsmin has a bug that will wrap lines.
#     # It will affect the subsequent multi-line processing logic, so deal with this situation in advance here.
#     template = re.sub(r'(^[\t ]*//[\s\S]*?\n)|(^[\t ]*/\*{1,2}[\s\S]*?\*/)', '', template, flags=re.M)
#     minified = jsmin(template)
#     try:
#         return shell_safe_json_parse(minified, preserve_order,
#                                      strict=False)  # use strict=False to allow multiline strings
#     except CLIError:
#         # Because the processing of removing comments and compression will lead to misplacement of error location,
#         # so the error message should be wrapped.
#         if file_path:
#             raise CLIError("Failed to parse '{}', please check whether it is a valid JSON format".format(file_path))
#         raise CLIError("Failed to parse the JSON data, please check whether it is a valid JSON format")
