-- Create a test agent for Strange Loop testing
INSERT INTO Agents (
    Id, 
    Name, 
    Type, 
    Status, 
    IsActive, 
    Configuration, 
    Version, 
    CreatedAt, 
    UpdatedAt, 
    IsDeleted
) VALUES (
    '550e8400-e29b-41d4-a716-446655440000',  -- Fixed GUID for consistency
    'UltraGenericSystem',
    'SelfEvolving',
    'Active',
    1,
    '{"evolutionEnabled": true, "maxEvolutions": 10, "capabilities": ["conversation", "code_generation", "self_analysis"]}',
    '1.0.0',
    datetime('now'),
    datetime('now'),
    0
); 