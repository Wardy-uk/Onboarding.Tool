import type { AuthenticationClaim } from "./AuthenticationClaim";

export interface AuthenticationState {
  authenticated: boolean;
  name: string;
  claims: AuthenticationClaim[];
}
