import * as coreClient from "@azure/core-client";
import * as coreAuth from "@azure/core-auth";
import { TeamCloudOptionalParams } from "./models";
export declare class TeamCloudContext extends coreClient.ServiceClient {
    $host: string;
    /**
     * Initializes a new instance of the TeamCloudContext class.
     * @param credentials Subscription credentials which uniquely identify client subscription.
     * @param $host server parameter
     * @param options The parameter options
     */
    constructor(credentials: coreAuth.TokenCredential, $host: string, options?: TeamCloudOptionalParams);
}
//# sourceMappingURL=teamCloudContext.d.ts.map