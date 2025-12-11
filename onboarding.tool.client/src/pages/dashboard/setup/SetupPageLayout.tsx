import { SetupPageContextProvider } from "./SetupContext";
import { SetupPage } from "./SetupPage";

export const SetupPageLayout = () => {
  return (
    <SetupPageContextProvider>
      <SetupPage />
    </SetupPageContextProvider>
  );
};
