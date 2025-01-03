﻿namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;

using Domain.Enums;
using Domain.Workflow;
using Entities.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateWorkflowHandler : IRequestHandler<CreateWorkflowRequest, Result<Workflow>>
{
    private readonly DataContext _context;

    public CreateWorkflowHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Workflow>> Handle(CreateWorkflowRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.AutoDetectChangesEnabled = false;


        var tenant = await _context.TenantEntities
            .FirstOrDefaultAsync(t => t.TenantEntityId == request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Fail("The tenant does not exist in the database. The pool cannot be created");
        }

        var workflowEntity = new WorkflowEntity
        {
            Name = "New Workflow",
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            TenantEntityId = tenant.TenantEntityId,
            WorkflowState = EWorkflowState.Inactive,
            ProcessFlowJson = null
        };

        if (request.ProcessFlow is not null)
        {
            workflowEntity.ProcessFlowJson = request.ProcessFlow.SerializeToJson();
            if(request.Name is not null)
            {
                workflowEntity.Name = request.Name;
            }
        }

        await _context.WorkflowEntities.AddAsync(workflowEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(workflowEntity.Map());
    }
}