import { useI18n } from "@/i18n/provider";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

export default function LanguageSwitcher() {
  const { lang, setLanguage } = useI18n();

  const onChange = async (v: string) => {
    await setLanguage(v);
  };

  return (
    <Select value={lang} onValueChange={onChange}>
      <SelectTrigger className="w-[120px]">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value="tr-TR">🇹🇷 TR</SelectItem>
        <SelectItem value="en-US">🇺🇸 EN</SelectItem>
      </SelectContent>
    </Select>
  );
}
