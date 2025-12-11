export type brandSettingType = "url" | "colour" | "string" | "number" | "boolean";

export interface brandSetting {
    key: string;
    label: string;
    type: brandSettingType;
    required: boolean;
}