/*
 * Copyright (c) Microsoft Corporation.
 * Licensed under the MIT License.
 *
 * Code generated by Microsoft (R) AutoRest Code Generator.
 * Changes may cause incorrect behavior and will be lost if the code is regenerated.
 */

import {
  OperationParameter,
  OperationURLParameter,
  OperationQueryParameter
} from "@azure/core-http";
import {
  ComponentDefinition as ComponentDefinitionMapper,
  ComponentTaskDefinition as ComponentTaskDefinitionMapper,
  DeploymentScopeDefinition as DeploymentScopeDefinitionMapper,
  DeploymentScope as DeploymentScopeMapper,
  OrganizationDefinition as OrganizationDefinitionMapper,
  UserDefinition as UserDefinitionMapper,
  User as UserMapper,
  ProjectDefinition as ProjectDefinitionMapper,
  ProjectIdentityDefinition as ProjectIdentityDefinitionMapper,
  ProjectIdentity as ProjectIdentityMapper,
  ProjectTemplateDefinition as ProjectTemplateDefinitionMapper,
  ProjectTemplate as ProjectTemplateMapper
} from "../models/mappers";

export const accept: OperationParameter = {
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

export const $host: OperationURLParameter = {
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

export const deleted: OperationQueryParameter = {
  parameterPath: ["options", "deleted"],
  mapper: {
    serializedName: "deleted",
    type: {
      name: "Boolean"
    }
  }
};

export const organizationId: OperationURLParameter = {
  parameterPath: "organizationId",
  mapper: {
    serializedName: "organizationId",
    required: true,
    type: {
      name: "String"
    }
  }
};

export const projectId: OperationURLParameter = {
  parameterPath: "projectId",
  mapper: {
    serializedName: "projectId",
    required: true,
    type: {
      name: "String"
    }
  }
};

export const contentType: OperationParameter = {
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

export const body: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: ComponentDefinitionMapper
};

export const id: OperationURLParameter = {
  parameterPath: "id",
  mapper: {
    serializedName: "id",
    required: true,
    type: {
      name: "String"
    }
  }
};

export const componentId: OperationURLParameter = {
  parameterPath: "componentId",
  mapper: {
    serializedName: "componentId",
    required: true,
    type: {
      name: "String"
    }
  }
};

export const body1: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: ComponentTaskDefinitionMapper
};

export const body2: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: DeploymentScopeDefinitionMapper
};

export const body3: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: DeploymentScopeMapper
};

export const body4: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: OrganizationDefinitionMapper
};

export const body5: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: UserDefinitionMapper
};

export const userId: OperationURLParameter = {
  parameterPath: "userId",
  mapper: {
    serializedName: "userId",
    required: true,
    type: {
      name: "String"
    }
  }
};

export const body6: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: UserMapper
};

export const body7: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: ProjectDefinitionMapper
};

export const body8: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: ProjectIdentityDefinitionMapper
};

export const projectIdentityId: OperationURLParameter = {
  parameterPath: "projectIdentityId",
  mapper: {
    serializedName: "projectIdentityId",
    required: true,
    type: {
      name: "String"
    }
  }
};

export const body9: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: ProjectIdentityMapper
};

export const body10: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: {
    serializedName: "body",
    type: {
      name: "Dictionary",
      value: { type: { name: "String" } }
    }
  }
};

export const tagKey: OperationURLParameter = {
  parameterPath: "tagKey",
  mapper: {
    serializedName: "tagKey",
    required: true,
    type: {
      name: "String"
    }
  }
};

export const body11: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: ProjectTemplateDefinitionMapper
};

export const projectTemplateId: OperationURLParameter = {
  parameterPath: "projectTemplateId",
  mapper: {
    serializedName: "projectTemplateId",
    required: true,
    type: {
      name: "String"
    }
  }
};

export const body12: OperationParameter = {
  parameterPath: ["options", "body"],
  mapper: ProjectTemplateMapper
};

export const trackingId: OperationURLParameter = {
  parameterPath: "trackingId",
  mapper: {
    serializedName: "trackingId",
    required: true,
    type: {
      name: "Uuid"
    }
  }
};
