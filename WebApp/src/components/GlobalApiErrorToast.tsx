import { useEffect } from "react";
import { toast } from "sonner";
import { useTranslation } from "react-i18next";

type ApiErrorEventDetail = {
  code: string;
  status?: number;
};

export function GlobalApiErrorToast() {
  const { t } = useTranslation();

  useEffect(() => {
    const handler = (e: Event) => {
      const ev = e as CustomEvent<ApiErrorEventDetail>;
      const code = ev.detail?.code ?? "common.unexpected_error";

      // status'a göre farklı davranış istersen:
      // if (ev.detail?.status === 403) toast.warning(t(code));
      // else toast.error(t(code));

      toast.error(t(code));
    };

    window.addEventListener("api:error", handler);
    return () => window.removeEventListener("api:error", handler);
  }, [t]);

  return null;
}
