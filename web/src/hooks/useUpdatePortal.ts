// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { api, onResponse } from "../API";
import { Organization } from "teamcloud";

export const useUpdatePortal = async (org?: Organization): Promise<Organization | undefined> => {

	if (org?.id) {

		const response = await api.updatePortal(org?.id, {
			onResponse: onResponse
		});

		return response.data;
	}

	return org;
}