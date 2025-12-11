import { BrandingPageContextProvider } from "./BrandingContextProvider";
import { BrandingPage } from "./BrandingPage";

export const BrandingLayout = () => {
  return (
    <BrandingPageContextProvider>
      <BrandingPage />
    </BrandingPageContextProvider>
  );
};

export default BrandingLayout;