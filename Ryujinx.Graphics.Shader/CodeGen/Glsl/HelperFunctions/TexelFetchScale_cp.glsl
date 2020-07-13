ivec2 Helper_TexelFetchScale(ivec2 inputVec, int samplerIndex) {
    float scale = cp_renderScale[1 + samplerIndex];
    if (scale == 1.0) {
        return inputVec;
    }
    if (scale < 0.0) { // If less than 0, try interpolate between texels by using the invocation id.
        return ivec2(vec2(inputVec) * (-scale) + mod(vec2(gl_GlobalInvocationID.xy), -scale));
    } else {
        return ivec2(vec2(inputVec) * scale);
    }
}