import { useState } from "react";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { postApi } from "@/lib/api";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Link } from "react-router-dom";

const schema = z.object({
  email: z.string().email("Invalid email"),
});
type FormData = z.infer<typeof schema>;

export function ForgotPassword() {
  const [done, setDone] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { register, handleSubmit, formState: { isSubmitting, errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: "" },
  });

  const onSubmit = async (data: FormData) => {
    setError(null);
    try {
      await postApi("/auth/forgot-password", { email: data.email.trim() });
      setDone(true);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Request failed");
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <Card className="rounded-2xl">
          <CardHeader>
            <CardTitle>Forgot password</CardTitle>
            <CardDescription>We’ll email you a reset link.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {done ? (
              <div className="space-y-3">
                <div className="text-sm">
                  If that email exists, we sent a reset link.
                </div>
                <Button asChild className="w-full">
                  <Link to="/login">Back to login</Link>
                </Button>
              </div>
            ) : (
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="email">Email</Label>
                  <Input id="email" type="email" autoComplete="email" {...register("email")} />
                  {errors.email && <p className="text-sm text-destructive">{errors.email.message}</p>}
                </div>

                {error && (
                  <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
                    {error}
                  </div>
                )}

                <Button className="w-full" disabled={isSubmitting}>
                  {isSubmitting ? "Sending..." : "Send reset link"}
                </Button>

                <div className="text-sm text-muted-foreground text-center">
                  <Link className="hover:underline" to="/login">Back to login</Link>
                </div>
              </form>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
