#!/usr/bin/env node
import { bumpWorkshop } from '@rimworks/mod-ci';

const stagePath = await bumpWorkshop({
  workshopId: process.env.WORKSHOP_ID || '3793646067',
  solution: 'Quickstarts.slnx',
});

console.log(`pushed from ${stagePath}`);
