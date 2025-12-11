import { Chip, Stack, Typography } from "@mui/material";
import { useBuildPageContext } from "../../../pages/dashboard/build/BuildContextProvider";
import { buildPortalAccount } from "../../../pages/dashboard/build/types/buildPortalAccount";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useSnackbar } from "../../../utils/SnackbarContext";

export const BuildPortalAccountsList = () => {
  const { selectedInstance } = useInstanceContext();
  const { portalAccounts, setPortalAccounts } = useBuildPageContext();
  const { showSnackbar } = useSnackbar();
  const api = new Api(env.apiBaseUrl);

  const deletePortalAccount = async (account: buildPortalAccount) => {
    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance?.host}/build/portal/delete/${account.id}`;

    const result: boolean = await api.post(endpoint, undefined, {
      credentials: "include",
    });

    if (result) {
      showSnackbar("Deleted portal account " + account.portalName, "success");
      setPortalAccounts((prev) => prev.filter((p) => p.id !== account.id));
    } else {
      showSnackbar(
        "Failed to delete portal account " + account.portalName,
        "error"
      );
    }
  };

  return (
    <>
      {portalAccounts.length === 0 ? (
        <Typography variant="body1">No Portal Accounts to display.</Typography>
      ) : (
        <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap" }}>
          {portalAccounts.map((a) => {
            return (
              <Chip
                key={a.id}
                label={a.portalName}
                onDelete={() => deletePortalAccount(a)}
                color="primary"
                sx={{ mb: 1 }}
              />
            );
          })}
        </Stack>
      )}
    </>
  );
};
