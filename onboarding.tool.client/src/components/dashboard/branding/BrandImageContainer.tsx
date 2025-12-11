import { Box, Button, CircularProgress, Typography } from "@mui/material";
import { ChangeEvent, FC, useEffect, useState } from "react";
import FileUploadIcon from "@mui/icons-material/FileUpload";
import { fileToBase64 } from "../../../utils/helpers/imageHelper";

interface BrandImageContainerProps {
  label: string;
  image: globalThis.File | null;
  onUpload: (file: globalThis.File) => void;
  width: number;
  height: number;
  uploading: boolean;
}

export const BrandImageContainer: FC<BrandImageContainerProps> = ({
  label,
  image,
  onUpload,
  width,
  height,
  uploading,
}) => {
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  useEffect(() => {
    if (image) {
      fileToBase64(image).then(setPreviewUrl);
    } else {
      setPreviewUrl(null);
    }
  }, [image]);

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) onUpload(file);
  };

  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        minWidth: width,
        position: "relative",
      }}
    >
      <Box
        sx={{
          width,
          height,
          border: "1px solid #ccc",
          borderRadius: 2,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          overflow: "hidden",
          mb: 1,
          background: "#fafafa",
          position: "relative",
        }}
      >
        {image ? (
          <img
            src={`data:image/png;base64,${previewUrl}`}
            alt={label}
            style={{ maxWidth: "100%", maxHeight: "100%" }}
          />
        ) : (
          <Typography variant="caption" color="textSecondary">
            No {label} provided
          </Typography>
        )}
        {uploading && (
          <Box
            sx={{
              position: "absolute",
              top: 0,
              left: 0,
              width: "100%",
              height: "100%",
              bgcolor: "rgba(255,255,255,0.7)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              zIndex: 2,
            }}
          >
            <CircularProgress size={40} />
          </Box>
        )}
      </Box>
      <Button
        variant="outlined"
        component="label"
        startIcon={<FileUploadIcon />}
        size="small"
        disabled={uploading}
      >
        Upload {label}
        <input
          type="file"
          accept={"image/png,image/jpeg"}
          hidden
          onChange={handleChange}
        />
      </Button>
    </Box>
  );
};
