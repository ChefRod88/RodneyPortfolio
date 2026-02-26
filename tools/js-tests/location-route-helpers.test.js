import { describe, expect, it } from 'vitest';
import { createRequire } from 'module';

const require = createRequire(import.meta.url);
const helpers = require('../../ChurchWebsite/wwwroot/js/location-route-helpers.js');

describe('location-route-helpers', () => {
  it('buildRouteLineAnimationState returns expected dash animation values', () => {
    const state = helpers.buildRouteLineAnimationState(125.5, 1800);

    expect(state).toEqual({
      strokeDasharray: '125.5',
      strokeDashoffsetStart: '125.5',
      strokeDashoffsetEnd: '0',
      transition: 'stroke-dashoffset 1800ms ease-out'
    });
  });

  it('buildRouteLineAnimationState returns null for invalid lengths', () => {
    expect(helpers.buildRouteLineAnimationState(NaN, 1800)).toBeNull();
    expect(helpers.buildRouteLineAnimationState(0, 1800)).toBeNull();
    expect(helpers.buildRouteLineAnimationState(-5, 1800)).toBeNull();
  });

  it('resetRouteLineAnimationStyle is idempotent and clears style values', () => {
    const styleTarget = {
      strokeDasharray: '99',
      strokeDashoffset: '12',
      transition: 'foo'
    };

    helpers.resetRouteLineAnimationStyle(styleTarget);
    helpers.resetRouteLineAnimationStyle(styleTarget);

    expect(styleTarget).toEqual({
      strokeDasharray: '',
      strokeDashoffset: '',
      transition: ''
    });
  });

  it('getViewportLayoutPolicy returns desktop square policy', () => {
    const policy = helpers.getViewportLayoutPolicy(1200, 768);
    expect(policy.isMobile).toBe(false);
    expect(policy.routePanelShape).toBe('square');
  });

  it('getViewportLayoutPolicy returns mobile bottom-sheet policy', () => {
    const policy = helpers.getViewportLayoutPolicy(480, 768);
    expect(policy.isMobile).toBe(true);
    expect(policy.routePanelShape).toBe('bottom-sheet');
  });

  it('calculateFitBoundsPadding returns mobile-aware bottom padding', () => {
    const padding = helpers.calculateFitBoundsPadding({ width: 340, height: 260 }, 480, 30);
    expect(padding).toEqual({
      paddingTopLeft: [30, 30],
      paddingBottomRight: [30, 284]
    });
  });

  it('calculateFitBoundsPadding returns desktop right-side panel padding', () => {
    const padding = helpers.calculateFitBoundsPadding({ width: 340, height: 300 }, 1280, 30);
    expect(padding).toEqual({
      paddingTopLeft: [30, 30],
      paddingBottomRight: [364, 189]
    });
  });
});
