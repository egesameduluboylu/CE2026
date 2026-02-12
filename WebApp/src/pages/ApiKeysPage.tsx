import { useI18n } from "@/i18n/provider";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { getApi, deleteApi } from "@/lib/api";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreHorizontal, Edit, Trash2, Key } from "lucide-react";

export default function ApiKeysPage() {
  const { t } = useI18n();
  const queryClient = useQueryClient();

  const { data: apiKeys, isLoading } = useQuery({
    queryKey: ["api-keys"],
    queryFn: async () => {
      const response = await getApi("/admin/api-keys");
      return response.data as any;
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (keyId: string) => {
      const response = await deleteApi(`/admin/api-keys/${keyId}`);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["api-keys"] });
    },
  });

  if (isLoading) {
    return <div className="flex items-center justify-center h-64">{t("common.loading")}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">{t("pages.api_keys")}</h1>
          <p className="text-gray-600">{t("descriptions.api_keys")}</p>
        </div>
        <Button>{t("api_keys.create_key")}</Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t("api_keys.key_list")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="mb-4">
            <Input
              placeholder={t("api_keys.search_placeholder")}
              className="max-w-sm"
            />
          </div>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("api_keys.name")}</TableHead>
                <TableHead>{t("api_keys.key")}</TableHead>
                <TableHead>{t("api_keys.status")}</TableHead>
                <TableHead>{t("api_keys.expires_at")}</TableHead>
                <TableHead>{t("api_keys.created_by")}</TableHead>
                <TableHead className="text-right">{t("common.actions")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {apiKeys?.map((apiKey: any) => (
                <TableRow key={apiKey.id}>
                  <TableCell>
                    <div className="flex items-center space-x-3">
                      <Key className="h-4 w-4" />
                      <div>
                        <div className="font-medium">{apiKey.name}</div>
                        <div className="text-sm text-gray-500">{apiKey.description}</div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <code className="bg-gray-100 px-2 py-1 rounded text-sm">
                      {apiKey.key.substring(0, 8)}...{apiKey.key.substring(apiKey.key.length - 8)}
                    </code>
                  </TableCell>
                  <TableCell>
                    <Badge variant={apiKey.isActive ? "default" : "secondary"}>
                      {apiKey.isActive ? t("api_keys.active") : t("api_keys.inactive")}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    {apiKey.expiresAt
                      ? new Date(apiKey.expiresAt).toLocaleDateString()
                      : t("api_keys.never_expires")}
                  </TableCell>
                  <TableCell>{apiKey.createdBy}</TableCell>
                  <TableCell className="text-right">
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="sm">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem>
                          <Edit className="mr-2 h-4 w-4" />
                          {t("common.edit")}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          className="text-red-600"
                          onClick={() => deleteMutation.mutate(apiKey.id)}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          {t("common.delete")}
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
