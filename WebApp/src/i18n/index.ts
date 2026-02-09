import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import LanguageDetector from "i18next-browser-languagedetector";

import en from "./locales/en/common.json";
import tr from "./locales/tr/common.json";

export const SUPPORTED = ["tr-TR", "en-GB"] as const;
export type SupportedCulture = (typeof SUPPORTED)[number];

const normalizeToSupported = (lng: string): SupportedCulture => {
  const l = (lng || "tr-TR").toLowerCase();
  if (l.startsWith("en")) return "en-GB";
  return "tr-TR";
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      "en-GB": { common: en },
      "tr-TR": { common: tr }
    },
    fallbackLng: "tr-TR",
    supportedLngs: SUPPORTED as unknown as string[],
    defaultNS: "common",
    ns: ["common"],
    interpolation: { escapeValue: false },
    detection: {
      order: ["localStorage", "navigator", "htmlTag"],
      lookupLocalStorage: "culture",
      caches: ["localStorage"]
    }
  });

export const getCulture = (): SupportedCulture => normalizeToSupported(i18n.language);

export const setCulture = async (culture: SupportedCulture) => {
  await i18n.changeLanguage(culture);
  localStorage.setItem("culture", culture);
};

export default i18n;
