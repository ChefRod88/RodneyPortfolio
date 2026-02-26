(function routeHelpersFactory(root, factory) {
  if (typeof module === 'object' && module.exports) {
    module.exports = factory();
    return;
  }
  root.LocationRouteHelpers = factory();
}(typeof globalThis !== 'undefined' ? globalThis : this, function buildHelpers() {
  function buildRouteLineAnimationState(totalLength, durationMs) {
    if (!Number.isFinite(totalLength) || totalLength <= 0) {
      return null;
    }
    return {
      strokeDasharray: String(totalLength),
      strokeDashoffsetStart: String(totalLength),
      strokeDashoffsetEnd: '0',
      transition: `stroke-dashoffset ${Math.max(0, durationMs || 1800)}ms ease-out`
    };
  }

  function resetRouteLineAnimationStyle(styleTarget) {
    if (!styleTarget) return;
    styleTarget.strokeDasharray = '';
    styleTarget.strokeDashoffset = '';
    styleTarget.transition = '';
  }

  function getViewportLayoutPolicy(viewportWidth, breakpoint) {
    const activeBreakpoint = Number.isFinite(breakpoint) ? breakpoint : 768;
    const width = Number.isFinite(viewportWidth) ? viewportWidth : 1024;
    const isMobile = width <= activeBreakpoint;
    return {
      isMobile,
      routePanelShape: isMobile ? 'bottom-sheet' : 'square'
    };
  }

  function calculateFitBoundsPadding(panelRect, viewportWidth, basePad) {
    const pad = Number.isFinite(basePad) ? basePad : 30;
    if (!panelRect || panelRect.width <= 0 || panelRect.height <= 0) {
      return {
        paddingTopLeft: [pad, pad],
        paddingBottomRight: [pad, pad]
      };
    }

    const policy = getViewportLayoutPolicy(viewportWidth, 768);
    if (policy.isMobile) {
      return {
        paddingTopLeft: [pad, pad],
        paddingBottomRight: [pad, Math.ceil(panelRect.height + 24)]
      };
    }

    return {
      paddingTopLeft: [pad, pad],
      paddingBottomRight: [Math.ceil(panelRect.width + 24), Math.ceil((panelRect.height * 0.55) + 24)]
    };
  }

  return {
    buildRouteLineAnimationState: buildRouteLineAnimationState,
    resetRouteLineAnimationStyle: resetRouteLineAnimationStyle,
    getViewportLayoutPolicy: getViewportLayoutPolicy,
    calculateFitBoundsPadding: calculateFitBoundsPadding
  };
}));
