export const splitString = (value: string, separator: string): string[] => {
return value.split(separator).map(item => item.trim());
}