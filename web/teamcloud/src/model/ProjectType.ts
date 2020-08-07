import { IIdentifiable, ITags, IProperties } from './index'

export interface ProjectType extends IIdentifiable, ITags, IProperties {
    isDefault: boolean;
    region: string;
    subscriptions: string[];
    subscriptionCapacity: number;
    resourceGroupNamePrefix?: string;
    providers: ProviderReference[];
}

export interface ProviderReference extends IIdentifiable, IProperties {
    dependsOn?: string[];
    metadata?: Record<string, Record<string, string>>;
}

