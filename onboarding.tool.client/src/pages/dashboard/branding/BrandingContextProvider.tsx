import { noop } from "lodash-es";
import { createContext, useContext, useState, type ReactNode } from "react";
import { brandBranch } from "./types/brandBranch";
import { brandSetting } from "./types/brandSetting";

interface BrandingPageContextType {
  selectedBrand: number;
  setSelectedBrand: (brand: number) => void;
  brands: brandBranch[];
  setBrands: (brands: brandBranch[]) => void;
  brandSettings: brandSetting[];
  setBrandSettings: (settings: brandSetting[]) => void;
  currentSettings: Record<string, string>;
  setCurrentSettings: (settings: Record<string, string>) => void;
}

const defaultContext: BrandingPageContextType = {
  selectedBrand: -1,
  setSelectedBrand: noop,
  brands: [],
  setBrands: noop,
  brandSettings: [],
  setBrandSettings: noop,
  currentSettings: {},
  setCurrentSettings: noop
};

const brandingPageContext =
  createContext<BrandingPageContextType>(defaultContext);

export function useBrandingPageContext() {
  return useContext(brandingPageContext);
}

export function BrandingPageContextProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [selectedBrand, setSelectedBrand] = useState<number>(
    defaultContext.selectedBrand
  );
  const [brands, setBrands] = useState<brandBranch[]>(defaultContext.brands);
  const [brandSettings, setBrandSettings] = useState<brandSetting[]>(defaultContext.brandSettings);
  const [currentSettings, setCurrentSettings] = useState<Record<string, string>>({});

  return (
    <brandingPageContext.Provider
      value={{
        selectedBrand,
        setSelectedBrand,
        brands,
        setBrands,
        brandSettings,
        setBrandSettings,
        currentSettings,
        setCurrentSettings
      }}
    >
      {children}
    </brandingPageContext.Provider>
  );
}
