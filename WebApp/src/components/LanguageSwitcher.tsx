import { useTranslation } from "react-i18next";
import { getCulture, setCulture, type SupportedCulture } from "../i18n";

export default function LanguageSwitcher() {
  const { t } = useTranslation();
  const current = getCulture();

  const onChange = async (v: string) => {
    await setCulture(v as SupportedCulture);
  };

  return (
    <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
      <span>{t("app.language")}:</span>
      <select value={current} onChange={(e) => onChange(e.target.value)}>
        <option value="tr-TR">TR</option>
        <option value="en-GB">EN</option>
      </select>
    </div>
  );
}
