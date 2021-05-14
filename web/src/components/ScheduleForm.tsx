// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { DefaultButton, Dropdown, DropdownMenuItemType, IconButton, IDropdownOption, PrimaryButton, Stack, Checkbox, Toggle, IComboBoxOption, ComboBox, IComboBox, Text } from '@fluentui/react';
import { Component, ComponentTaskTemplate, ComponentTemplate, ScheduleDefinition } from 'teamcloud';
import { ContentContainer, ContentHeader, ContentProgress, ContentSeparator } from '.';
import { useOrg, useProject, useProjectComponentTemplates, useProjectComponents, useProjectMembers, useUser, useCreateProjectSchedule } from '../hooks';
import { ProjectMember } from '../model';

export const ScheduleForm: React.FC = () => {

    const history = useHistory();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [formEnabled, setFormEnabled] = useState<boolean>(true);

    const [componentTaskOptions, setComponentTaskOptions] = useState<IDropdownOption[]>();

    const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    const now = new Date();

    const [timeOptions] = useState<IComboBoxOption[]>(
        [...Array(24).keys()]
            .flatMap(h => [...Array(60).keys()].filter(m => m % 5 === 0).map(m => new Date(now.getFullYear(), now.getMonth(), now.getDate(), h, m, 0, 0)))
            .map(d => ({ key: d.toUTCString(), text: d.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' }), data: d })));

    const [member, setMember] = useState<ProjectMember>();

    const [availableComponents, setAvailableComponents] = useState<{ component: Component, template?: ComponentTemplate }[]>();

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

    const createSchedule = useCreateProjectSchedule();


    // const timezone = now.toTimeString().slice(9, 15).replace('GMT', 'UTC') + ':' + now.toTimeString().slice(15);
    // const timezone = now.toLocaleTimeString([], { hour12: false, hour: '2-digit', minute: '2-digit', timeZoneName: 'short' }).slice(6) + ' (' + now.toTimeString().slice(9, 15).replace('GMT', 'UTC') + ':' + now.toTimeString().slice(15, 17) + ')';
    const timezone = now.toTimeString().slice(19, -1) + ' (' + now.toTimeString().slice(9, 15).replace('GMT', 'UTC') + ':' + now.toTimeString().slice(15, 17) + ')';


    // const theme = getTheme();

    // const _scheduleComplete = () => timeDate && daysOfWeek && daysOfWeek.length > 0 && componentTasks && componentTasks.length > 0;
    const _scheduleComplete = () => true;

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
                    const _tasks = c.template.tasks.map(t => ({ key: t.id!, text: t.displayName ?? t.id!, data: { component: c.component, template: c.template!, taskTemplate: t } }));
                    _options.push(..._tasks);
                }
            });
            setComponentTaskOptions(_options);
        }
    }, [availableComponents, componentTaskOptions]);


    const _submitForm = async () => {
        if (timeDate && componentTasks && _scheduleComplete()) {
            setFormEnabled(false);

            let days = daysOfWeek;

            const localDate = timeDate.getDate();
            const utcDate = timeDate.getUTCDate();

            if (localDate > utcDate) {
                days = daysOfWeek.map(d => d === 0 ? 6 : d - 1);
            } else if (localDate < utcDate) {
                days = daysOfWeek.map(d => d === 6 ? 0 : d + 1);
            }

            const scheduleDef: ScheduleDefinition = {
                enabled: enabled,
                recurring: recurring,
                utcHour: timeDate.getUTCHours(),
                utcMinute: timeDate.getUTCMinutes(),
                daysOfWeek: days.map(d => dayNames[d]),
                componentTasks: componentTasks.map(ct => ({ componentId: ct.component.id, componentTaskTemplateId: ct.taskTemplate.id }))
            }

            console.log(JSON.stringify(scheduleDef));

            await createSchedule(scheduleDef);
            setFormEnabled(true);
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
        if (componentTasks && option?.data)
            setComponentTasks([...componentTasks, option.data])
    };

    const _updateDaysOfWeek = (dayIndex: number, checked?: boolean) => {
        if (checked === undefined || dayIndex < 0 || dayIndex > 6) return;

        if (checked && !daysOfWeek.includes(dayIndex))
            setDaysOfWeek([...daysOfWeek, dayIndex]);
        else if (!checked && daysOfWeek.includes(dayIndex))
            setDaysOfWeek(daysOfWeek.filter(d => d !== dayIndex))
    }

    // const _dayCheckBoxes = () => [...Array(7).keys()].map(i => (<Stack.Item><Checkbox label={dayNames[i]} checked={daysOfWeek.includes(i)} onChange={(_, checked) => _updateDaysOfWeek(i, checked)} /></Stack.Item>))

    return (
        <>
            {/* <ContentProgress progressHidden={formEnabled && !orgIsLoading && !projectIsLoading && !userIsLoading && !membersIsLoading && !componentsIsLoading && !templatesIsLoading} />
            <ContentHeader title='New Schedule'>
                <IconButton iconProps={{ iconName: 'ChromeClose' }}
                    onClick={() => history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/settings/schedules`)} />
            </ContentHeader>
            <ContentContainer> */}
            {/* <ScheduleForm /> */}


            <Stack tokens={{ childrenGap: '40px' }}>
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
                            <Stack horizontal tokens={{ childrenGap: '12px' }}>
                                {[...Array(7).keys()].map(i => (<Stack.Item key={i}><Checkbox key={i} label={dayNames[i]} checked={daysOfWeek.includes(i)} onChange={(_, checked) => _updateDaysOfWeek(i, checked)} /></Stack.Item>))}
                                {/* {_dayCheckBoxes()} */}
                                {/* <Stack.Item><Checkbox label={dayNames[0]} checked={daysOfWeek.includes(0)} onChange={(_, checked) => _updateDaysOfWeek(0, checked)} /></Stack.Item>
                                <Stack.Item><Checkbox label={dayNames[1]} checked={daysOfWeek.includes(1)} onChange={(_, checked) => _updateDaysOfWeek(1, checked)} /></Stack.Item>
                                <Stack.Item><Checkbox label={dayNames[2]} checked={daysOfWeek.includes(2)} onChange={(_, checked) => _updateDaysOfWeek(2, checked)} /></Stack.Item>
                                <Stack.Item><Checkbox label={dayNames[3]} checked={daysOfWeek.includes(3)} onChange={(_, checked) => _updateDaysOfWeek(3, checked)} /></Stack.Item>
                                <Stack.Item><Checkbox label={dayNames[4]} checked={daysOfWeek.includes(4)} onChange={(_, checked) => _updateDaysOfWeek(4, checked)} /></Stack.Item>
                                <Stack.Item><Checkbox label={dayNames[5]} checked={daysOfWeek.includes(5)} onChange={(_, checked) => _updateDaysOfWeek(5, checked)} /></Stack.Item>
                                <Stack.Item><Checkbox label={dayNames[6]} checked={daysOfWeek.includes(6)} onChange={(_, checked) => _updateDaysOfWeek(6, checked)} /></Stack.Item> */}
                            </Stack>
                        </Stack.Item>
                        <Stack.Item>
                            <ContentSeparator />
                        </Stack.Item>
                        <Stack.Item >
                            {/* <Stack horizontal grow verticalAlign='end' tokens={{ childrenGap: '12px' }}> */}
                            {/* <Stack.Item> */}
                            <ComboBox
                                required
                                label='Time'
                                disabled={!formEnabled}
                                options={timeOptions || []}
                                selectedKey={timeDate?.toUTCString()}
                                styles={{ root: { maxWidth: '400px' }, optionsContainerWrapper: { maxHeight: '400px' } }}
                                onChange={_onTimeComboBoxChange} />
                            {/* </Stack.Item> */}
                            {/* <Stack.Item> */}
                            <Text variant='small' >{timezone}</Text>
                            {/* <Toggle label='Recurring' checked={recurring} onChange={(_, checked) => setRecurring(checked ?? false)} /> */}
                            {/* </Stack.Item> */}
                            {/* </Stack> */}
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
                                onChange={_onComponentDropdownChange} />
                        </Stack.Item>
                        <Stack.Item styles={{ root: { paddingTop: '24px' } }}>
                            <PrimaryButton text='Create schedule' disabled={!formEnabled || !_scheduleComplete()} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
                            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
                        </Stack.Item>
                    </Stack>
                </Stack.Item>
            </Stack>
            {/* </ContentContainer> */}
        </>
    );
}
