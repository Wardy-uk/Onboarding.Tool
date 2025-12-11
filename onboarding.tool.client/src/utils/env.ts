export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || "",
} as const;

export type Config = typeof env;