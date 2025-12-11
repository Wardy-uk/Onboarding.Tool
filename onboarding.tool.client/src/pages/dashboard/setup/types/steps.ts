import { setupStep } from "./setupStep";
import { setupTemplateSteps } from "./setupTemplateSteps";

export interface steps {
  templateSteps: setupTemplateSteps;
  additionalSteps: setupStep[];
}
