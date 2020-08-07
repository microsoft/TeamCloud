import { IIdentifiable } from "./index";

export interface AzureResourceGroup extends IIdentifiable {
    name: string;
    subscriptionId: string;
    region: string;
}
