import { buildBranch } from "./buildBranch";
import { buildPortalAccount } from "./buildPortalAccount";

export interface buildOverview {
    branches: buildBranch[];
    portalAccounts: buildPortalAccount[];
}