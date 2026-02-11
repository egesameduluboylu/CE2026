import { useTranslation } from "react-i18next";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
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
import { MoreHorizontal, Edit, Trash2, Building, Users } from "lucide-react";

export default function TenantsPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: tenants, isLoading } = useQuery({
    queryKey: ["tenants"],
    queryFn: async () => {
      const response = await fetch("/api/admin/tenants");
      if (!response.ok) throw new Error("Failed to fetch tenants");
      return response.json();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (tenantId: string) => {
      const response = await fetch(`/api/admin/tenants/${tenantId}`, {
        method: "DELETE",
      });
      if (!response.ok) throw new Error("Failed to delete tenant");
      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tenants"] });
    },
  });

  if (isLoading) {
    return <div className="flex items-center justify-center h-64">{t("common.loading")}</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">{t("pages.tenants")}</h1>
          <p className="text-gray-600">{t("descriptions.tenants")}</p>
        </div>
        <Button>{t("tenants.create_tenant")}</Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t("tenants.tenant_list")}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="mb-4">
            <Input
              placeholder={t("tenants.search_placeholder")}
              className="max-w-sm"
            />
          </div>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("tenants.name")}</TableHead>
                <TableHead>{t("tenants.domain")}</TableHead>
                <TableHead>{t("tenants.status")}</TableHead>
                <TableHead>{t("tenants.users")}</TableHead>
                <TableHead>{t("tenants.plan")}</TableHead>
                <TableHead>{t("tenants.created_at")}</TableHead>
                <TableHead className="text-right">{t("common.actions")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {tenants?.map((tenant: any) => (
                <TableRow key={tenant.id}>
                  <TableCell>
                    <div className="flex items-center space-x-3">
                      <Building className="h-4 w-4" />
                      <div>
                        <div className="font-medium">{tenant.name}</div>
                        <div className="text-sm text-gray-500">{tenant.description}</div>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>{tenant.domain}</TableCell>
                  <TableCell>
                    <Badge variant={tenant.isActive ? "default" : "secondary"}>
                      {tenant.isActive ? t("tenants.active") : t("tenants.inactive")}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center space-x-2">
                      <Users className="h-4 w-4" />
                      <span>{tenant.userCount || 0}</span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline">{tenant.plan || t("tenants.free_plan")}</Badge>
                  </TableCell>
                  <TableCell>
                    {new Date(tenant.createdAt).toLocaleDateString()}
                  </TableCell>
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
                          onClick={() => deleteMutation.mutate(tenant.id)}
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
