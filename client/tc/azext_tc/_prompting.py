# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=too-many-lines
# pylint: disable=line-too-long

from knack.log import get_logger
from knack.prompting import verify_is_a_tty

logger = get_logger(__name__)

# used knack prompting as starting reference:


def _input(msg):
    return input(msg)


def prompt_number(msg, help_string=None):
    verify_is_a_tty()

    while True:
        value = _input(msg)
        if value == '?' and help_string is not None:
            print(help_string)
            continue
        try:
            return float(value)
        except ValueError:
            logger.warning('%s is not a valid number', value)


def prompt_array(msg, item_type, help_string=None):
    verify_is_a_tty()

    while True:
        value = _input('{}\nPlease enter one or more space-seperated {}s: '.format(msg, item_type))
        if value == '?' and help_string is not None:
            print(help_string)
            continue
        ans = []
        values = value.split()
        for v in values:
            try:
                if item_type == 'string':
                    ans.append(value)
                elif item_type == 'boolean':
                    ans.append(value.lower() == 'true')
                elif item_type == 'integer':
                    v_int = int(value)
                    ans.append(v_int)
                elif item_type == 'number':
                    v_float = float(value)
                    ans.append(v_float)
                else:
                    logger.warning("Unrecognized type '%s'. Interpretting as string.", item_type)
                    ans.append(value)
            except ValueError:
                logger.warning('%s is not a valid %s', v, item_type)
        return ans


def prompt_multi_choice_list(msg, a_list, default=1, help_string=None):
    """Prompt user to select from a list of possible choices.

    :param msg:A message displayed to the user before the choice list
    :type msg: str
    :param a_list:The list of choices (list of strings or list of dicts with 'name' & 'desc')
    "type a_list: list
    :param default:The default option that should be chosen if user doesn't enter a choice
    :type default: int
    :returns: A list of indexs of the items chosen.
    """
    verify_is_a_tty()
    options = '\n'.join([' [{}] {}{}'
                         .format(i + 1,
                                 x['name'] if isinstance(x, dict) and 'name' in x else x,
                                 ' - ' + x['desc'] if isinstance(x, dict) and 'desc' in x else '')
                         for i, x in enumerate(a_list)])
    allowed_vals = list(range(1, len(a_list) + 1))
    while True:
        val = _input('{}\n{}\nPlease enter one or more space-seperated choices [Default choice({})]: '.format(
            msg, options, default))
        if val == '?' and help_string is not None:
            print(help_string)
            continue
        if not val:
            val = '{}'.format(default)
        try:
            anss = []
            vals = val.split()
            for v in vals:
                ans = int(v)
                if ans in allowed_vals:
                    # array index is 0-based, user input is 1-based
                    anss.append(ans - 1)
                else:
                    raise ValueError
            return anss
        except ValueError:
            logger.warning('Valid values are %s', allowed_vals)
