import * as coreHttp from "@azure/core-http";
import { TeamCloudOptionalParams } from "./models";
export declare class TeamCloudContext extends coreHttp.ServiceClient {
    $host: string;
    /**
     * Initializes a new instance of the TeamCloudContext class.
     * @param credentials Subscription credentials which uniquely identify client subscription.
     * @param $host server parameter
     * @param options The parameter options
     */
    constructor(credentials: coreHttp.TokenCredential | coreHttp.ServiceClientCredentials, $host: string, options?: TeamCloudOptionalParams);
}
//# sourceMappingURL=teamCloudContext.d.ts.map