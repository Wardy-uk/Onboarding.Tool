import { createTheme } from '@mui/material/styles';

declare module '@mui/material/styles' {
  interface Palette {
    status: {
      primary: string;
      success: string;
      warning: string;
      danger: string;
    };
  }

  interface PaletteOptions {
    status?: {
      primary?: string;
      success?: string;
      warning?: string;
      danger?: string;
    };
  }
}

const theme = createTheme({
  palette: {
    status: {
      primary: '#0E8A7D',
      success: '#0E8A7D',
      warning: '#ED6C02',
      danger: '#D50000',
    },
    primary: {
      main: '#0E8A7D'
    },
    success: {
      main: '#0E8A7D'
    },
    warning: {
      main: '#ED6C02'
    },
    error: {
      main: '#D50000'
    }
  },
});

export default theme;
