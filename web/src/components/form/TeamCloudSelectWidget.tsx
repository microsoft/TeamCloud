// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from "react";
import { Dropdown, IDropdownOption } from "@fluentui/react";
import { WidgetProps } from "@rjsf/core";
import _pick from "lodash/pick";

import "./TeamCloudSelectWidget.css"

// Keys of IDropdownProps from @fluentui/react
const allowedProps = [
  "placeHolder",
  "options",
  "onChange",
  "onChanged",
  "onRenderLabel",
  "onRenderPlaceholder",
  "onRenderPlaceHolder",
  "onRenderTitle",
  "onRenderCaretDown",
  "dropdownWidth",
  "responsiveMode",
  "defaultSelectedKeys",
  "selectedKeys",
  "multiselectDelimiter",
  "notifyOnReselect",
  "isDisabled",
  "keytipProps",
  "theme",
  "styles",

  // ISelectableDroppableTextProps
  "componentRef",
  "label",
  "ariaLabel",
  "id",
  "className",
  "defaultSelectedKey",
  "selectedKey",
  "multiSelect",
  "options",
  "onRenderContainer",
  "onRenderList",
  "onRenderItem",
  "onRenderOption",
  "onDismiss",
  "disabled",
  "required",
  "calloutProps",
  "panelProps",
  "errorMessage",
  "placeholder",
  "openOnKeyboardFocus"
];

export const TeamCloudSelectWidget = ({
  schema,
  uiSchema,
  id,
  options,
  label,
  formContext,
  required,
  disabled,
  readonly,
  value,
  multiple,
  autofocus,
  onChange,
  onBlur,
  onFocus,
}: WidgetProps) => {
  const { enumOptions, enumDisabled } = options;

  const _onChange = (
    _ev?: React.FormEvent<HTMLElement>,
    item?: IDropdownOption
  ) => {
    if (!item) {
      return;
    }
    if (multiple) {
      const valueOrDefault = value || [];
      if (item.selected) {
        onChange([...valueOrDefault, item.key]);
      } else {
        onChange(valueOrDefault.filter((key: any) => key !== item.key));
      }
    } else {
      onChange(item.key);
    }
  };
  
  const _onBlur = (e: any) => onBlur(id, e.target.value);

  const _onFocus = (e: any) => onFocus(id, e.target.value);

  const newOptions = (enumOptions as {value: any, label: any}[]).map(option => ({
    key: option.value,
    text: option.label,
    disabled: (enumDisabled as any[] || []).indexOf(option.value) !== -1
  }));

  const uiProps = _pick(options.props || {}, allowedProps);

  console.log(options);

  return (	  
    <>
      <Dropdown
        multiSelect={multiple}
        defaultSelectedKey={multiple ? undefined : value}
		    defaultSelectedKeys={multiple ? value : undefined}
        required={required}
        label={(options?.title as string) || schema?.title as string || label}
        options={newOptions}
        disabled={disabled || readonly}
        onChange={_onChange}
        onBlur={_onBlur}
        onFocus={_onFocus}
        {...uiProps}
      />
      { options.description 
        ? <span><span className="ms-TextField-description teamCloudSelectWidgetDescription">{options.description}</span></span> 
        : <></> }
    </>
  );
};
