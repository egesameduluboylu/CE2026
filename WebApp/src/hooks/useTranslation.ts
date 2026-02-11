import { useTranslation } from "react-i18next";

export const useAppTranslation = () => {
  const { t, i18n } = useTranslation("common");
  
  return {
    t,
    currentLanguage: i18n.language,
    changeLanguage: i18n.changeLanguage,
    isTurkish: i18n.language.startsWith("tr"),
    isEnglish: i18n.language.startsWith("en")
  };
};

export type AppTranslation = ReturnType<typeof useAppTranslation>;
