import { Api } from "../../../../utils/api";
import { env } from "../../../../utils/env";
import { setupResult } from "./setupResult";

const api = new Api(env.apiBaseUrl);

export interface setupAdditionalStep {
  key: string;
  label: string;
  onSetup: (instance: string) => Promise<setupResult>;
  onSuccess: (response: setupResult) => string;
}

export const setupAdditionalSteps: setupAdditionalStep[] = [
  {
    key: "setupDefaultPrint",
    label: "Add Default Print Libraries",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/print-libraries`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Default Print] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupBrands",
    label: "Create Brands",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/brands`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Create Brands] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupBranches",
    label: "Create Branches",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/branches`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Create Branches] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupDeliveryAddresses",
    label: "Create Delivery Addresses",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/delivery`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Create Delivery Addresses] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupUsers",
    label: "Create Users",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/users`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Create Users] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupRss",
    label: "Add RSS Feeds",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/rss`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Add RSS] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupRobocop",
    label: "Add Robocop Settings",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/configuration`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Add Robocop Settings] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupScheduledReports",
    label: "Add Scheduled Reports",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/reporting`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Add Reports] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupComponents",
    label: "Add Email Components",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/components`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Add Components] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupAutomatedEmails",
    label: "Add Automated Emails",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/automation`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Add Email Triggers] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupAutomated2020s",
    label: "Add Automated 2020s",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/briefyourmarket/setup/${instance}/2020s`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Add Print Triggers] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupBuildMilestones",
    label: "Add Build Milestones",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/buildyourmarket/${instance}/milestones`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Add Build Milestones] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupBuildPortals",
    label: "Add Build Portals",
    onSetup: async (instance: string) => {
      return await api.post(`/api/v1/buildyourmarket/${instance}/portals`, {
        id: instance,
      });
    },
    onSuccess: (response: setupResult) => {
      return `[Add Build Portals] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupBuildBranches",
    label: "Add Build Branches",
    onSetup: async (instance: string) => {
      return await api.post(`/api/v1/buildyourmarket/${instance}/branches`, {
        id: instance,
      });
    },
    onSuccess: (response: setupResult) => {
      return `[Add Build Branches] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupBuildContent",
    label: "Add Build Content",
    onSetup: async (instance: string) => {
      return await api.post(`/api/v1/buildyourmarket/${instance}/content`, {
        id: instance,
      });
    },
    onSuccess: (response: setupResult) => {
      return `[Add Build Content] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
  {
    key: "setupMatchToCrm",
    label: "Add Match to CRM",
    onSetup: async (instance: string) => {
      return await api.post(
        `/api/v1/buildyourmarket/${instance}/matchtocrm`,
        undefined,
        { credentials: "include" }
      );
    },
    onSuccess: (response: setupResult) => {
      return `[Add CRM Matcher] ${response.success ? "Success" : "Error"}: ${response.message}`;
    },
  },
];
