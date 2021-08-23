/*
 * Copyright (c) Microsoft Corporation.
 * Licensed under the MIT License.
 *
 * Code generated by Microsoft (R) AutoRest Code Generator.
 * Changes may cause incorrect behavior and will be lost if the code is regenerated.
 */
import { ComponentDefinition as ComponentDefinitionMapper, ComponentTaskDefinition as ComponentTaskDefinitionMapper, DeploymentScopeDefinition as DeploymentScopeDefinitionMapper, DeploymentScope as DeploymentScopeMapper, OrganizationDefinition as OrganizationDefinitionMapper, UserDefinition as UserDefinitionMapper, User as UserMapper, ProjectDefinition as ProjectDefinitionMapper, ProjectIdentityDefinition as ProjectIdentityDefinitionMapper, ProjectIdentity as ProjectIdentityMapper, ProjectTemplateDefinition as ProjectTemplateDefinitionMapper, ProjectTemplate as ProjectTemplateMapper, ScheduleDefinition as ScheduleDefinitionMapper, Schedule as ScheduleMapper } from "../models/mappers";
export const accept = {
    parameterPath: "accept",
    mapper: {
        defaultValue: "application/json",
        isConstant: true,
        serializedName: "Accept",
        type: {
            name: "String"
        }
    }
};
export const $host = {
    parameterPath: "$host",
    mapper: {
        serializedName: "$host",
        required: true,
        type: {
            name: "String"
        }
    },
    skipEncoding: true
};
export const deleted = {
    parameterPath: ["options", "deleted"],
    mapper: {
        serializedName: "deleted",
        type: {
            name: "Boolean"
        }
    }
};
export const organizationId = {
    parameterPath: "organizationId",
    mapper: {
        serializedName: "organizationId",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const projectId = {
    parameterPath: "projectId",
    mapper: {
        serializedName: "projectId",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const contentType = {
    parameterPath: ["options", "contentType"],
    mapper: {
        defaultValue: "application/json",
        isConstant: true,
        serializedName: "Content-Type",
        type: {
            name: "String"
        }
    }
};
export const body = {
    parameterPath: ["options", "body"],
    mapper: ComponentDefinitionMapper
};
export const componentId = {
    parameterPath: "componentId",
    mapper: {
        serializedName: "componentId",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const body1 = {
    parameterPath: ["options", "body"],
    mapper: ComponentTaskDefinitionMapper
};
export const id = {
    parameterPath: "id",
    mapper: {
        serializedName: "id",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const body2 = {
    parameterPath: ["options", "body"],
    mapper: DeploymentScopeDefinitionMapper
};
export const deploymentScopeId = {
    parameterPath: "deploymentScopeId",
    mapper: {
        serializedName: "deploymentScopeId",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const body3 = {
    parameterPath: ["options", "body"],
    mapper: DeploymentScopeMapper
};
export const contentType1 = {
    parameterPath: ["options", "contentType"],
    mapper: {
        defaultValue: "application/json-patch+json",
        isConstant: true,
        serializedName: "Content-Type",
        type: {
            name: "String"
        }
    }
};
export const timeRange = {
    parameterPath: ["options", "timeRange"],
    mapper: {
        serializedName: "timeRange",
        type: {
            name: "String"
        }
    }
};
export const commands = {
    parameterPath: ["options", "commands"],
    mapper: {
        serializedName: "commands",
        type: {
            name: "Sequence",
            element: {
                type: {
                    name: "String"
                }
            }
        }
    }
};
export const commandId = {
    parameterPath: "commandId",
    mapper: {
        serializedName: "commandId",
        required: true,
        type: {
            name: "Uuid"
        }
    }
};
export const expand = {
    parameterPath: ["options", "expand"],
    mapper: {
        serializedName: "expand",
        type: {
            name: "Boolean"
        }
    }
};
export const body4 = {
    parameterPath: ["options", "body"],
    mapper: OrganizationDefinitionMapper
};
export const body5 = {
    parameterPath: ["options", "body"],
    mapper: UserDefinitionMapper
};
export const userId = {
    parameterPath: "userId",
    mapper: {
        serializedName: "userId",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const body6 = {
    parameterPath: ["options", "body"],
    mapper: UserMapper
};
export const body7 = {
    parameterPath: ["options", "body"],
    mapper: ProjectDefinitionMapper
};
export const body8 = {
    parameterPath: ["options", "body"],
    mapper: ProjectIdentityDefinitionMapper
};
export const projectIdentityId = {
    parameterPath: "projectIdentityId",
    mapper: {
        serializedName: "projectIdentityId",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const body9 = {
    parameterPath: ["options", "body"],
    mapper: ProjectIdentityMapper
};
export const body10 = {
    parameterPath: ["options", "body"],
    mapper: {
        serializedName: "body",
        type: {
            name: "Dictionary",
            value: { type: { name: "String" } }
        }
    }
};
export const tagKey = {
    parameterPath: "tagKey",
    mapper: {
        serializedName: "tagKey",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const body11 = {
    parameterPath: ["options", "body"],
    mapper: ProjectTemplateDefinitionMapper
};
export const projectTemplateId = {
    parameterPath: "projectTemplateId",
    mapper: {
        serializedName: "projectTemplateId",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const body12 = {
    parameterPath: ["options", "body"],
    mapper: ProjectTemplateMapper
};
export const body13 = {
    parameterPath: ["options", "body"],
    mapper: ScheduleDefinitionMapper
};
export const scheduleId = {
    parameterPath: "scheduleId",
    mapper: {
        serializedName: "scheduleId",
        required: true,
        type: {
            name: "String"
        }
    }
};
export const body14 = {
    parameterPath: ["options", "body"],
    mapper: ScheduleMapper
};
export const trackingId = {
    parameterPath: "trackingId",
    mapper: {
        serializedName: "trackingId",
        required: true,
        type: {
            name: "Uuid"
        }
    }
};
//# sourceMappingURL=parameters.js.map