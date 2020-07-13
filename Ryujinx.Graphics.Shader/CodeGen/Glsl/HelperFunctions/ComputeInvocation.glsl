uvec3 Helper_LocalInvocationID() {
    float scale = cp_renderScale[0];
    
    if (scale == 1.0) {
        return gl_LocalInvocationID;
    }
    
    // Local invocation scaling.
    uvec2 scaledInvocationID = uvec2(vec2(gl_GlobalInvocationID.xy) / scale);

    return uvec3(mod(scaledInvocationID, gl_WorkGroupSize.xy), gl_LocalInvocationID.z);
}

uvec3 Helper_WorkGroupID() {
    float scale = cp_renderScale[0];
    
    if (scale == 1.0) {
        return gl_WorkGroupID;
    }
    
    // Work group scaling.
    uvec2 scaledInvocationID = uvec2(vec2(gl_GlobalInvocationID.xy) / scale);

    return uvec3(scaledInvocationID / gl_WorkGroupSize.xy, gl_WorkGroupID.z);
}