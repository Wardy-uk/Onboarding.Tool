export interface branch {
  id: number | null;
  isDefault: boolean;
  name: string;
  salesEmail: string;
  salesPhone: string;
  lettingsEmail: string;
  lettingsPhone: string;
  address: address;
}

export interface address {
  id: number | null;
  address1: string;
  address2: string | null;
  address3: string | null;
  town: string;
  postCode1: string;
  postCode2: string;
}
