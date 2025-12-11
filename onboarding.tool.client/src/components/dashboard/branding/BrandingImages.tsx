import {
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  Paper,
  Skeleton,
  Typography,
} from "@mui/material";
import { BrandImageContainer } from "./BrandImageContainer";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { brandImage, brandImageType } from "../../../pages/dashboard/branding/types/brandImage";
import { useEffect, useState } from "react";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { base64ToFile, fileToBase64 } from "../../../utils/helpers/imageHelper";
import { BrandCardPreview } from "./BrandCardPreview";

export const BrandingImages = () => {
  const { loading, selectedInstance } = useInstanceContext();
  const [logoFile, setLogoFile] = useState<globalThis.File | null>(null);
  const [logoFileUploading, setLogoFileUploading] = useState<boolean>(false);
  const [splashFile, setSplashFile] = useState<globalThis.File | null>(null);
  const [splashFileUploading, setSplashFileUploading] =
    useState<boolean>(false);
  const [alternateLogoFile, setAlternateLogoFile] =
    useState<globalThis.File | null>(null);
  const [alternateLogoFileUploading, setAlternateLogoFileUploading] =
    useState<boolean>(false);
  const [previewOpen, setPreviewOpen] = useState<boolean>(false);
  const [requireAlternateLogo, setRequireAlternateLogo] =
    useState<boolean>(false);
  const { showSnackbar } = useSnackbar();
  const api = new Api(env.apiBaseUrl);

  const handleAlternateLogoCheckbox = async () => {
    if (!selectedInstance) return;

    if (requireAlternateLogo) {

      if (alternateLogoFile != null) {
        const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance.host}/image/preview/remove-alternate-logo`;
        await api.post(endpoint, undefined, { credentials: "include" });
        setAlternateLogoFile(null);
      }
  
      setRequireAlternateLogo(false);
    } else {
      setRequireAlternateLogo(true);
    }
  };

  const fetchImage = async (type: brandImageType) => {
    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance?.host}/image/${type}`;
    const result: string = await api.get(endpoint, undefined, {
      credentials: "include",
    });

    if (result) {
      const file: File = base64ToFile(result, "file");

      if (type == brandImageType.logo) {
        setLogoFile(file);
      } else if (type === brandImageType.splash) {
        setSplashFile(file);
      } else if (type == brandImageType.logoAlternate) {
        setRequireAlternateLogo(true);
        setAlternateLogoFile(file);
      }
    }
  };

  const handleImageUpload = async (type: brandImageType, file: File) => {
    if (!["image/png", "image/jpeg"].includes(file.type)) {
      alert("Only PNG and JPEG images are allowed.");
      return;
    }

    if (type == brandImageType.logo) {
      setLogoFile(file);
      setLogoFileUploading(true);
    } else if (type == brandImageType.splash) {
      setSplashFile(file);
      setSplashFileUploading(true);
    } else if (type == brandImageType.logoAlternate) {
      setAlternateLogoFile(file);
      setAlternateLogoFileUploading(true);
    } else {
      return;
    }

    const b64content = await fileToBase64(file);
    await finishUpload(type, { fileName: file.name, data: b64content });
  };

  const finishUpload = async (type: brandImageType, image: brandImage) => {
    if (!selectedInstance) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance?.host}/image/upload/${type}`;
    const result = await api.post(endpoint, image, { credentials: "include" });

    if (result) {
      showSnackbar(`${brandImageType[type]} uploaded successfully.`, "success");
    } else {
      showSnackbar(`Failed to upload ${type}`, "error");
    }

    if (type == brandImageType.logo) {
      setLogoFileUploading(false);
    } else if (type == brandImageType.splash) {
      setSplashFileUploading(false);
    } else if (type == brandImageType.logoAlternate) {
      setAlternateLogoFileUploading(false);
    }
  };

  useEffect(() => {
    if (selectedInstance) {
      setLogoFile(null);
      setSplashFile(null);
      setRequireAlternateLogo(false);
      setAlternateLogoFile(null);

      fetchImage(brandImageType.logo);
      fetchImage(brandImageType.splash);
      fetchImage(brandImageType.logoAlternate);
    }
  }, [selectedInstance]);

  return (
    <Paper variant="outlined" sx={{ mt: 3, p: 2 }}>
      {loading || selectedInstance == null ? (
        <Skeleton />
      ) : (
        <>
          <Typography variant="h6" gutterBottom sx={{ fontWeight: "bold" }}>
            Images
          </Typography>
          <Box sx={{ display: "flex", gap: 4, mb: 1, flexWrap: "wrap" }}>
            <BrandImageContainer
              width={120}
              height={120}
              image={logoFile}
              onUpload={(upload: File) =>
                handleImageUpload(brandImageType.logo, upload)
              }
              label="logo"
              uploading={logoFileUploading}
            />
            <BrandImageContainer
              width={220}
              height={120}
              image={splashFile}
              onUpload={(upload: File) =>
                handleImageUpload(brandImageType.splash, upload)
              }
              label="splash"
              uploading={splashFileUploading}
            />
          </Box>
          <Box sx={{ display: "flex", gap: 4, mb: 1, flexWrap: "wrap" }}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={requireAlternateLogo}
                  onChange={handleAlternateLogoCheckbox}
                />
              }
              label="Require Alternate Logo"
            />
          </Box>
          <Box sx={{ textAlign: 'center', mb: 4 }}>
            {requireAlternateLogo && (
              <>
                <BrandImageContainer
                  width={120}
                  height={120}
                  image={alternateLogoFile}
                  onUpload={(upload: File) =>handleImageUpload(brandImageType.logoAlternate, upload)}
                  label="alternative logo"
                  uploading={alternateLogoFileUploading}
                />
                <Typography variant="caption" sx={{ display: 'block', mt: 2 }}>Alternative logos are to be used when the logo would otherwise appear (partially) invisible on things like email headers, or cards, due to theme colour conflicts.</Typography>
                <Typography variant="caption" sx={{ display: 'block' }}>This should typically be a greyscale version of the logo.</Typography>
              </>
            )}
          </Box>
          <Box sx={{ display: "flex", flexDirection: "row-reverse" }}>
            <Button
              variant="contained"
              color="primary"
              onClick={() => setPreviewOpen(true)}
            >
              Preview Logo on Print Card
            </Button>
          </Box>
          <BrandCardPreview
            open={previewOpen}
            onCancel={() => setPreviewOpen(false)}
          />
        </>
      )}
    </Paper>
  );
};
