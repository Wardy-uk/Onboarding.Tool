import { BuildPageContextProvider } from "./BuildContextProvider";
import BuildPage from "./BuildPage";

export const BuildPageLayout = () => {
  return (
    <BuildPageContextProvider>
      <BuildPage />
    </BuildPageContextProvider>
  );
};

export default BuildPageLayout;
