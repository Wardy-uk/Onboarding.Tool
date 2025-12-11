import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Link,
  Typography,
} from "@mui/material";
import { FC, useRef, useState } from "react";
import { userImport } from "../../../pages/dashboard/users/types/userImport";
import { csvHasRequiredHeaders } from "../../../utils/helpers/validationHelper";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { user } from "../../../pages/dashboard/users/types/user";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { useUsersPageContext } from "../../../pages/dashboard/users/UsersContextProvider";

interface UserImportDialogProps {
  open: boolean;
  onCancel: () => void;
  onImport: () => void;
}

const requiredFields: string[] = ["email"];

function parseCSV(text: string): { data: userImport[]; errors: string[] } {
  const lines = text.trim().split(/\r?\n/);
  if (lines.length < 2)
    return {
      data: [],
      errors: [
        "Your CSV must include at least two lines with the first being the headers.",
      ],
    };

  const rawHeaders = lines[0].split(",").map((h) => h.trim());
  const normalizedHeaders = rawHeaders.map((h) => h.toLowerCase());

  const headerMap: Record<string, string> = {};
  requiredFields.forEach((f) => {
    const idx = normalizedHeaders.findIndex((h) => h === f.toLowerCase());
    if (idx !== -1) headerMap[rawHeaders[idx]] = f;
  });

  const issuesWithHeaders: string[] = csvHasRequiredHeaders(
    Object.values(headerMap).map((h) => h.toLowerCase()),
    requiredFields.map((f) => f.toLowerCase())
  );
  if (issuesWithHeaders.length > 0)
    return { data: [], errors: issuesWithHeaders };

  let errors: string[] = [];
  const data = lines.slice(1).map((line, idx) => {
    const values = line.split(",");
    const row: any = {};

    rawHeaders.forEach((h, i) => {
      const key = headerMap[h];
      if (key) row[key] = values[i]?.trim() || "";
    });

    requiredFields.forEach((f) => {
      if (!row[f]) {
        errors.push(`Row ${idx + 2}: Missing value for '${f}'`);
      }
    });

    if (row.email && !/^\S+@\S+\.\S+$/.test(row.email)) {
      errors.push(`Row ${idx + 2}: Invalid email in 'email'`);
    }

    return row as userImport;
  });

  return { data, errors };
}

export const UserImportDialog: FC<UserImportDialogProps> = ({
  open,
  onCancel,
  onImport,
}) => {
  const { loading, selectedInstance } = useInstanceContext();
  const { setUsers } = useUsersPageContext();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [csvErrors, setCsvErrors] = useState<string[]>([]);
  const [csvData, setCsvData] = useState<userImport[] | null>(null);
  const api = new Api(env.apiBaseUrl);
  const { showSnackbar } = useSnackbar();

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const text = await file.text();
    const { data, errors } = parseCSV(text);
    setCsvData(errors.length === 0 ? data : null);
    setCsvErrors(errors);
  };

  const handleUpload = async () => {
    if (csvData == null || selectedInstance == null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance.host}/user/import`;
    const result: user[] = await api.post(
      endpoint,
      csvData,
      { credentials: "include" }
    );

    if (result.length == 0) {
      showSnackbar("Failed to import any users", "error");
      return;
    } else if (result.length != csvData.length) {
      showSnackbar("One or more users have failed to import", "warning");
    } else {
      showSnackbar("Import complete", "success");
    }

    setUsers((prev) => [...prev, ...result]);
    setCsvData(null);
    onImport();
  };

  return (
    <Dialog open={open} onClose={onCancel} maxWidth="sm" fullWidth>
      <DialogTitle>Import Users</DialogTitle>
      <DialogContent>
        <Box mb={2}>
          <Typography gutterBottom>
            Please upload a CSV file with the following required column:
          </Typography>
          <Typography component="span">email *</Typography>
          <Typography gutterBottom>
            Any column marked with an asterisk (*) is required.
          </Typography>
          <Link
            href={`${import.meta.env.VITE_BASE_URL || "/"}sample-users.csv`}
            download
          >
            Download sample CSV
          </Link>
        </Box>
        <Box sx={{ display: "flex", justifyContent: "flex-end" }}>
          <Button
            variant="outlined"
            component="label"
            disabled={loading || selectedInstance == null}
          >
            Select CSV File
            <input
              type="file"
              accept=".csv"
              hidden
              ref={fileInputRef}
              onChange={handleFileChange}
              disabled={loading || selectedInstance == null}
            />
          </Button>
        </Box>
        {csvErrors.length > 0 && (
          <Box mt={2}>
            {csvErrors.map((err, i) => (
              <Alert severity="error" key={i}>
                {err}
              </Alert>
            ))}
          </Box>
        )}
        {csvData && csvErrors.length === 0 && (
          <Alert severity="success" sx={{ mt: 2 }}>
            CSV looks good! Ready to upload {csvData.length} users.
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button
          onClick={onCancel}
          disabled={loading || selectedInstance == null}
        >
          Cancel
        </Button>
        <Button
          onClick={handleUpload}
          variant="contained"
          color="primary"
          disabled={
            loading ||
            selectedInstance == null ||
            !csvData ||
            csvErrors.length > 0
          }
        >
          Import
        </Button>
      </DialogActions>
    </Dialog>
  );
};
