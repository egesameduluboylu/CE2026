import { Languages, Check } from "lucide-react";
import { useTranslation } from "react-i18next";
import { getCulture, setCulture, type SupportedCulture } from "@/i18n";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const LANGS: Array<{ value: SupportedCulture; label: string }> = [
  { value: "tr-TR", label: "Türkçe" },
  { value: "en-GB", label: "English" },
];

export function LanguageMenu() {
  const { t } = useTranslation();
  const current = getCulture();

  const change = async (v: SupportedCulture) => {
    await setCulture(v);
    // istersen burada toast vs
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="h-9 w-9 rounded-xl" aria-label={t("app.language")}>
          <Languages className="h-4 w-4" />
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" sideOffset={8} className="w-40 p-1">
        {LANGS.map((l) => (
          <DropdownMenuItem
            key={l.value}
            onSelect={(e) => {
              e.preventDefault();
              change(l.value);
            }}
            className="flex items-center justify-between"
          >
            <span>{l.label}</span>
            {current === l.value ? <Check className="h-4 w-4" /> : null}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
