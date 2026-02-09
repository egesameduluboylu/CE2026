import { cn } from "@/shared/lib/cn";

type PageProps = {
  title: string;
  description?: string;
  actions?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
};

export function Page({
  title,
  description,
  actions,
  children,
  className,
}: PageProps) {
  return (
    <div className={cn("space-y-6", className)}>
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
          {description && (
            <p className="text-sm text-muted-foreground">{description}</p>
          )}
        </div>
        {actions && <div className="flex gap-2">{actions}</div>}
      </div>

      <div className="rounded-2xl border bg-card p-4 sm:p-6">
        {children}
      </div>
    </div>
  );
}
