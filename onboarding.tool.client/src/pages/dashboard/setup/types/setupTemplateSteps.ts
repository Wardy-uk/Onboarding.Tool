export enum templateState {
  incomplete,
  unconfirmed,
  complete,
  queued,
}

export interface setupTemplateSteps {
  templatesConfirmed: templateState;
  directMailConfirmed: templateState;
  letterheadConfirmed: templateState;
}
