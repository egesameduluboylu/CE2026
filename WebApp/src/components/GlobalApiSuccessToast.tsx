import { useEffect } from "react";
import { toast } from "sonner";
import { useTranslation } from "react-i18next";

type ApiSuccessEventDetail = {
  code: string;          // i18n key
  status?: number;
};

export function GlobalApiSuccessToast() {
  const { t } = useTranslation();

  useEffect(() => {
    const handler = (e: Event) => {
      const ev = e as CustomEvent<ApiSuccessEventDetail>;
      const code = ev.detail?.code ?? "common.success";

      toast.success(t(code));
    };

    window.addEventListener("api:success", handler);
    return () => window.removeEventListener("api:success", handler);
  }, [t]);

  return null;
}
