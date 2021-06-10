// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { User } from 'teamcloud';
import { GraphPrincipal } from './GraphPrincipal';

export interface Member {
    user: User;
    graphPrincipal?: GraphPrincipal;
}
