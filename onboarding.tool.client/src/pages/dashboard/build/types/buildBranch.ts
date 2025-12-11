import { buildDistrict } from "./buildDistrict";

export interface buildBranch {
  id: number;
  name: string;
  districts: buildDistrict[];
}
