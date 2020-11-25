// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { GraphUser } from '.';
import { User } from 'teamcloud';

export interface Member {
    user: User;
    graphUser?: GraphUser;
}
