import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.rodneyachery.portfolio',
  appName: 'Rodney Chery Portfolio',
  webDir: 'www',
  server: {
    url: 'https://www.rodneyachery.com',
    cleartext: false,
  },
};

export default config;
