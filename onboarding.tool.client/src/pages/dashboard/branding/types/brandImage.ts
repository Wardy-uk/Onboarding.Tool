export enum brandImageType {
  logo = 0,
  splash = 1,
  printLogo = 2,
  logoAlternate = 3,
  printLogoAlternate = 4,
}

export interface brandImage {
  fileName: string;
  data: string;
}
