export interface Scripts {
  name: string;
  src: string;
}
export const ScriptStore: Scripts[] = [
  {
    name: 'tableau',
    src: 'https://tableau-live.eastus.cloudapp.azure.com/javascripts/api/tableau-2.9.1.min.js',
  }
];
