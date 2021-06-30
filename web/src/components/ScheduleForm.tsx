// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { DefaultButton, Dropdown, DropdownMenuItemType, IDropdownOption, PrimaryButton, Stack, Checkbox, Toggle, IComboBoxOption, ComboBox, IComboBox, Text, Label } from '@fluentui/react';
import { Component, ComponentTaskTemplate, ComponentTemplate, ScheduleDefinition } from 'teamcloud';
import { useOrg, useProject, useProjectComponentTemplates, useProjectComponents, useProjectMembers, useUser, useCreateProjectSchedule, useProjectSchedule, useUpdateProjectSchedule } from '../hooks';
import { DaysOfWeek, DaysOfWeekNames, ProjectMember, shiftToLocal, shiftToUtc } from '../model';
import { ContentSeparator } from '.';
import { ErrorBar } from './common';

export const ScheduleForm: React.FC = () => {

    const history = useHistory();

    const { orgId, projectId, itemId } = useParams() as { orgId: string, projectId: string, itemId: string };

    const now = new Date();

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [errorResult, setErrorResult] = useState<any>();

    const [componentTaskOptions, setComponentTaskOptions] = useState<IDropdownOption[]>();

    const [timeOptions] = useState<IComboBoxOption[]>(
        [...Array(24).keys()]
            .flatMap(h => [...Array(60).keys()].filter(m => m % 5 === 0).map(m => new Date(now.getFullYear(), now.getMonth(), now.getDate(), h, m, 0, 0)))
            .map(d => ({ key: ((d.getHours() * 60) + d.getMinutes()), text: d.toTimeDisplayString(false), data: d })));

    const [member, setMember] = useState<ProjectMember>();

    const [availableComponents, setAvailableComponents] = useState<{ component: Component, template?: ComponentTemplate }[]>();

    const [isNew, setIsNew] = useState(true);
    const [daysOfWeek, setDaysOfWeek] = useState([1, 2, 3, 4, 5]);
    const [timeDate, setTimeDate] = useState<Date>();
    const [enabled, setEnabled] = useState(true);
    const [recurring, setRecurring] = useState(true);
    const [componentTasks, setComponentTasks] = useState<{ component: Component, template: ComponentTemplate, taskTemplate: ComponentTaskTemplate }[]>([]);

    const { data: org } = useOrg();
    const { data: user } = useUser();
    const { data: project } = useProject();
    const { data: members } = useProjectMembers();
    const { data: components } = useProjectComponents();
    const { data: templates } = useProjectComponentTemplates();
    const { data: schedule } = useProjectSchedule();

    const createSchedule = useCreateProjectSchedule();
    const updateSchedule = useUpdateProjectSchedule();

    const timezone = now.toTimeZoneString();

    const _scheduleComplete = () => timeDate && daysOfWeek && daysOfWeek.length > 0 && componentTasks && componentTasks.length > 0;
    // const _scheduleComplete = () => true;

    useEffect(() => {
        if (itemId && schedule && components && templates && isNew) {
            console.log('setIsNew(false);');

            const refDate = new Date();
            refDate.setUTCHours(schedule.utcHour!, schedule.utcMinute, 0, 0);

            const days = shiftToLocal(schedule.daysOfWeek!, refDate);

            const _tasks = schedule.componentTasks?.map(ct => {
                const _component = components.find(c => c.id === ct.componentId)!;
                const _template = templates.find(t => t.id === _component.templateId)!;
                const _taskTemplate = _template.tasks!.find(tt => tt.id === ct.componentTaskTemplateId)!;
                return { component: _component, template: _template, taskTemplate: _taskTemplate };
            }) ?? [];

            setDaysOfWeek(days.indices);
            setTimeDate(refDate);
            setEnabled(schedule.enabled ?? true);
            setRecurring(schedule.recurring ?? true);
            setComponentTasks(_tasks);

            setIsNew(false);
        }
    }, [isNew, itemId, schedule, components, templates]);


    useEffect(() => {
        if (user && members && !member) {
            console.log(`+ setMember`);
            setMember(members.find(m => m.user.id === user.id));
        }
    }, [user, member, members]);


    useEffect(() => {
        if (member && components && templates && !availableComponents) {
            console.log(`+ setAvailableComponents`);
            const _orgRole = member.user.role.toLowerCase();
            const _projectRole = member.projectMembership.role.toLowerCase();

            const _isAdmin = _orgRole === 'owner' || _orgRole === 'admin' || _projectRole === 'owner' || _projectRole === 'admin';

            if (_isAdmin) {
                setAvailableComponents(components.map(c => ({ component: c, template: templates.find(t => t.id === c.templateId) })));
            } else {
                setAvailableComponents(components.filter(c => c.creator === member.user.id).map(c => ({ component: c, template: templates.find(t => t.id === c.templateId) })));
            }
        }
    }, [member, components, templates, availableComponents]);


    useEffect(() => {
        if (availableComponents && !componentTaskOptions) {
            console.log(`+ setComponentTaskOptions`);
            const _options: IDropdownOption[] = [];
            availableComponents.forEach(c => {
                _options.push({ key: c.component.id, text: c.component.displayName ?? c.component.slug, itemType: DropdownMenuItemType.Header });
                if (c.template?.tasks) {
                    const _tasks = c.template.tasks.map(t => ({ key: `${c.component.id}${t.id!}`, text: t.displayName ?? t.id!, data: { component: c.component, template: c.template!, taskTemplate: t } }));
                    _options.push(..._tasks);
                }
            });
            setComponentTaskOptions(_options);
        }
    }, [availableComponents, componentTaskOptions]);


    const _submitForm = async () => {
        if (timeDate && componentTasks && _scheduleComplete()) {
            setFormEnabled(false);

            const days = shiftToUtc(daysOfWeek, timeDate);

            try {

                if (isNew) {

                    const scheduleDef: ScheduleDefinition = {
                        enabled: enabled,
                        recurring: recurring,
                        utcHour: timeDate.getUTCHours(),
                        utcMinute: timeDate.getUTCMinutes(),
                        daysOfWeek: days.names,
                        componentTasks: componentTasks.map(ct => ({ componentId: ct.component.id, componentTaskTemplateId: ct.taskTemplate.id }))
                    }

                    // console.log(JSON.stringify(scheduleDef));
                    await createSchedule(scheduleDef);


                } else if (schedule) {

                    schedule.enabled = enabled;
                    schedule.recurring = recurring;
                    schedule.utcHour = timeDate.getUTCHours();
                    schedule.utcMinute = timeDate.getUTCMinutes();
                    schedule.daysOfWeek = days.names;
                    schedule.componentTasks = componentTasks.map(ct => ({ componentId: ct.component.id, componentTaskTemplateId: ct.taskTemplate.id }));

                    await updateSchedule(schedule);
                }

                setFormEnabled(true);

            } catch (error) {
                setErrorResult(error);
                setFormEnabled(true);
            }
        }
    };

    const _resetAndCloseForm = () => {
        setFormEnabled(true);
        history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/settings/schedules`);
    };

    const _onTimeComboBoxChange = (event: React.FormEvent<IComboBox>, option?: IComboBoxOption, index?: number, value?: string): void => {
        if (option?.data instanceof Date) {
            setTimeDate(option.data);
        }
    };

    const _onComponentDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        if (componentTasks === undefined || option?.data === undefined || option?.selected === undefined) return;

        const data = option.data as { component: Component, template: ComponentTemplate, taskTemplate: ComponentTaskTemplate };

        const item = componentTasks.find(ct => !!ct.taskTemplate.id && (ct.component.id === data.component.id && ct.taskTemplate.id === data.taskTemplate.id));

        if (option.selected && !item)
            setComponentTasks([...componentTasks, data]);
        else if (!option.selected && !!item)
            setComponentTasks(componentTasks.filter(ct => !!ct.taskTemplate.id && !(ct.component.id === data.component.id && ct.taskTemplate.id === data.taskTemplate.id)));
    };

    const _updateDaysOfWeek = (dayIndex: number, checked?: boolean) => {
        if (checked === undefined || dayIndex < 0 || dayIndex > 6) return;

        if (checked && !daysOfWeek.includes(dayIndex))
            setDaysOfWeek([...daysOfWeek, dayIndex]);
        else if (!checked && daysOfWeek.includes(dayIndex))
            setDaysOfWeek(daysOfWeek.filter(d => d !== dayIndex))
    }

    return (
        <Stack tokens={{ childrenGap: '40px' }}>
            <ErrorBar stackItem error={errorResult} />
            <Stack.Item grow styles={{ root: { minWidth: '40%', } }}>
                <Stack styles={{ root: { paddingTop: '20px' } }} tokens={{ childrenGap: '20px' }}>
                    <Stack.Item>
                        <Stack horizontal tokens={{ childrenGap: '60px' }}>
                            <Stack.Item>
                                <Toggle label='Enabled' checked={enabled} onChange={(_, checked) => setEnabled(checked ?? false)} />
                            </Stack.Item>
                            <Stack.Item>
                                <Toggle label='Recurring' checked={recurring} onChange={(_, checked) => setRecurring(checked ?? false)} />
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
                    <Stack.Item>
                        <ContentSeparator />
                    </Stack.Item>
                    <Stack.Item>
                        <Label required>Days of the week</Label>
                        <Stack horizontal styles={{ root: { paddingTop: '8px' } }} tokens={{ childrenGap: '12px' }}>
                            {DaysOfWeek
                                .map(i => (
                                    <Stack.Item key={i}>
                                        <Checkbox key={i} label={DaysOfWeekNames[i]} checked={daysOfWeek.includes(i)} onChange={(_, checked) => _updateDaysOfWeek(i, checked)} />
                                    </Stack.Item>)
                                )}
                        </Stack>
                    </Stack.Item>
                    <Stack.Item>
                        <ContentSeparator />
                    </Stack.Item>
                    <Stack.Item >
                        <ComboBox
                            required
                            label='Time'
                            disabled={!formEnabled}
                            options={timeOptions || []}
                            selectedKey={timeDate ? ((timeDate.getHours() * 60) + timeDate.getMinutes()) : undefined}
                            styles={{ root: { maxWidth: '400px' }, optionsContainerWrapper: { maxHeight: '400px' } }}
                            onChange={_onTimeComboBoxChange} />
                        <Text variant='small' >{timezone}</Text>
                    </Stack.Item>
                    <Stack.Item>
                        <ContentSeparator />
                    </Stack.Item>
                    <Stack.Item>
                        <Dropdown
                            required
                            label='Component Tasks'
                            disabled={!formEnabled}
                            options={componentTaskOptions || []}
                            styles={{ root: { maxWidth: '400px' }, dropdownItemsWrapper: { maxHeight: '400px' } }}
                            multiSelect
                            selectedKeys={componentTasks.filter(ct => !!ct.taskTemplate.id).map(ct => `${ct.component.id}${ct.taskTemplate.id!}`)}
                            onChange={_onComponentDropdownChange} />
                    </Stack.Item>
                    <Stack.Item styles={{ root: { paddingTop: '24px' } }}>
                        <PrimaryButton text={isNew ? 'Create schedule' : 'Update schedule'} disabled={!formEnabled || !_scheduleComplete()} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
                        <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
        </Stack>
    );
}
