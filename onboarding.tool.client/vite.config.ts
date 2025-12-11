import { fileURLToPath, URL } from 'node:url';
import { defineConfig, loadEnv } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';

export default defineConfig(({ mode }) => {
  // Load env variables based on the current mode
  const env = loadEnv(mode, process.cwd(), '');

  const isDocker = false;

  const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ''
      ? `${process.env.APPDATA}/ASP.NET/https`
      : `${process.env.HOME}/.aspnet/https`;

  const certificateName = "onboarding.tool.client";
  const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
  const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

  if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true });
  }

  if (!isDocker) {
    if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
      if (
        0 !==
        child_process.spawnSync(
          'dotnet',
          [
            'dev-certs',
            'https',
            '--export-path',
            certFilePath,
            '--format',
            'Pem',
            '--no-password',
          ],
          { stdio: 'inherit' }
        ).status
      ) {
        throw new Error('Could not create certificate.');
      }
    }
  }

  const target = process.env.ASPNETCORE_HTTPS_PORT
    ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
    : process.env.ASPNETCORE_URLS
    ? process.env.ASPNETCORE_URLS.split(';')[0]
    : 'https://localhost:7154';

  return {
    plugins: [plugin()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    server: {
      proxy: {
        '^/weatherforecast': {
          target,
          secure: false,
        },
      },
      port: parseInt(process.env.DEV_SERVER_PORT || '60193'),
      https: isDocker
        ? undefined
        : {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
          },
    },
    // Use your VITE_BASE_URL env variable here
    base: env.VITE_BASE_URL || '/',
  };
});
