// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Model.SystemModeller;

public record EntityModelJsonConfigResponse(string Json) : GenericJsonResponse(Json);

public class EntityModelConfigResponse
{
    public IEnumerable<EntityModel> Result { get; set; } = Array.Empty<EntityModel>();
}

